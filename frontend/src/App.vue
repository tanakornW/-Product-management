<script setup>
import { computed, onMounted, reactive, watch } from 'vue'

const apiBaseUrl = import.meta.env.VITE_API_BASE_URL ?? 'http://localhost:5271'
const productCodePattern = /^[A-Z0-9]{5}(-[A-Z0-9]{5}){5}$/

const state = reactive({
  products: [],
  loading: false,
  searchTerm: '',
  error: '',
})

const addModal = reactive({
  visible: false,
  code: '',
  submitting: false,
  error: '',
})

const qrModal = reactive({
  visible: false,
  loading: false,
  product: null,
  imageUrl: '',
  error: '',
})

const deleteModal = reactive({
  visible: false,
  product: null,
  loading: false,
  error: '',
})

let searchDebounceHandle

const canAddSubmit = computed(
  () => productCodePattern.test(addModal.code) && !addModal.submitting,
)

const formatProductCode = (value) => {
  const sanitized = (value ?? '')
    .toUpperCase()
    .replace(/[^A-Z0-9]/g, '')
    .slice(0, 30)

  if (!sanitized) {
    return ''
  }

  const parts = []
  for (let i = 0; i < sanitized.length; i += 5) {
    parts.push(sanitized.substring(i, i + 5))
  }

  return parts.join('-')
}

const fetchProducts = async () => {
  state.loading = true
  state.error = ''
  try {
    const qs = state.searchTerm
      ? `?search=${encodeURIComponent(state.searchTerm.trim())}`
      : ''
    const response = await fetch(`${apiBaseUrl}/api/Products${qs}`)
    if (!response.ok) {
      throw new Error('ไม่สามารถดึงข้อมูลได้')
    }

    state.products = await response.json()
  } catch (error) {
    console.error(error)
    state.error = 'เกิดข้อผิดพลาดในการโหลดข้อมูล'
  } finally {
    state.loading = false
  }
}

const openAddModal = () => {
  addModal.visible = true
  addModal.code = ''
  addModal.error = ''
}

const closeAddModal = () => {
  addModal.visible = false
  addModal.code = ''
  addModal.submitting = false
  addModal.error = ''
}

const submitNewProduct = async () => {
  addModal.code = formatProductCode(addModal.code)
  if (!productCodePattern.test(addModal.code)) {
    addModal.error = 'รหัสสินค้าต้องอยู่ในรูปแบบ xxxxx-xxxxx-... จำนวน 30 ตัวอักษร'
    return
  }

  addModal.submitting = true
  addModal.error = ''

  try {
    const response = await fetch(`${apiBaseUrl}/api/Products`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify({ code: addModal.code }),
    })

    if (!response.ok) {
      const payload = await response.json().catch(() => null)
      throw new Error(payload?.message || 'บันทึกรหัสสินค้าไม่สำเร็จ')
    }

    closeAddModal()
    await fetchProducts()
  } catch (error) {
    addModal.error = error.message
  } finally {
    addModal.submitting = false
  }
}

const openQrModal = async (product) => {
  qrModal.visible = true
  qrModal.loading = true
  qrModal.product = product
  qrModal.error = ''

  if (qrModal.imageUrl) {
    URL.revokeObjectURL(qrModal.imageUrl)
    qrModal.imageUrl = ''
  }

  try {
    const response = await fetch(`${apiBaseUrl}/api/Products/${product.id}/qr`)
    if (!response.ok) {
      throw new Error('ไม่สามารถสร้าง QR Code ได้')
    }

    const blob = await response.blob()
    qrModal.imageUrl = URL.createObjectURL(blob)
  } catch (error) {
    qrModal.error = error.message
  } finally {
    qrModal.loading = false
  }
}

const closeQrModal = () => {
  qrModal.visible = false
  if (qrModal.imageUrl) {
    URL.revokeObjectURL(qrModal.imageUrl)
    qrModal.imageUrl = ''
  }
}

const promptDelete = (product) => {
  deleteModal.visible = true
  deleteModal.product = product
  deleteModal.error = ''
}

const cancelDelete = () => {
  deleteModal.visible = false
  deleteModal.product = null
  deleteModal.error = ''
}

const confirmDelete = async () => {
  if (!deleteModal.product) return

  deleteModal.loading = true
  deleteModal.error = ''

  try {
    const response = await fetch(
      `${apiBaseUrl}/api/Products/${deleteModal.product.id}`,
      { method: 'DELETE' },
    )
    if (!response.ok) {
      throw new Error('ลบข้อมูลไม่สำเร็จ')
    }

    cancelDelete()
    await fetchProducts()
  } catch (error) {
    deleteModal.error = error.message
  } finally {
    deleteModal.loading = false
  }
}

const handleAddInput = (event) => {
  addModal.code = formatProductCode(event.target.value)
}

const restrictAddInput = (event) => {
  const controlKeys = [
    'Backspace',
    'Delete',
    'ArrowLeft',
    'ArrowRight',
    'Tab',
    'Home',
    'End',
    'Enter',
  ]

  if (controlKeys.includes(event.key)) {
    return
  }

  if (!/^[a-zA-Z0-9-]$/.test(event.key)) {
    event.preventDefault()
    return
  }

  const plainLength = addModal.code.replace(/-/g, '').length
  const isLetterOrDigit = /^[a-zA-Z0-9]$/.test(event.key)

  if (isLetterOrDigit && plainLength >= 30) {
    event.preventDefault()
  }
}

watch(
  () => state.searchTerm,
  () => {
    if (searchDebounceHandle) {
      clearTimeout(searchDebounceHandle)
    }
    searchDebounceHandle = setTimeout(() => {
      fetchProducts()
    }, 300)
  },
)

const formatDate = (value) => {
  if (!value) return '-'
  return new Intl.DateTimeFormat('th-TH', {
    dateStyle: 'short',
    timeStyle: 'short',
  }).format(new Date(value))
}

onMounted(() => {
  fetchProducts()
})
</script>

<template>
  <div class="app-shell">
    <header class="app-header">
      <h1>รายการรหัสสินค้า</h1>
      <p>เพิ่ม / ค้นหา / สร้าง QR Code และลบรหัสสินค้า</p>
    </header>

    <section class="card">
      <div class="search-bar">
        <div class="field">
          <label for="search-input">ค้นหารหัสสินค้า</label>
          <input
            id="search-input"
            type="text"
            v-model.trim="state.searchTerm"
            placeholder="พิมพ์เพื่อค้นหา..."
          />
        </div>
        <div class="search-actions">
          <button type="button" class="ghost" @click="fetchProducts">
            รีเฟรช
          </button>
          <button type="button" class="primary add-button" @click="openAddModal">
            Add
          </button>
        </div>
      </div>

      <p v-if="state.error" class="error-message">{{ state.error }}</p>
      <p v-if="state.loading" class="info-message">กำลังโหลดข้อมูล...</p>

      <div class="table-wrapper">
        <table>
          <thead>
            <tr>
              <th>#</th>
              <th>รหัสสินค้า</th>
              <th>สร้างเมื่อ</th>
              <th class="actions-column">การทำงาน</th>
            </tr>
          </thead>
          <tbody>
            <tr v-if="!state.loading && state.products.length === 0">
              <td colspan="4" class="empty-row">ยังไม่มีข้อมูล</td>
            </tr>
            <tr v-for="(product, index) in state.products" :key="product.id">
              <td>{{ index + 1 }}</td>
              <td class="code-cell">{{ product.code }}</td>
              <td>{{ formatDate(product.createdAt) }}</td>
              <td class="actions-column">
                <button type="button" class="info" @click="openQrModal(product)">
                  QR
                </button>
                <button
                  type="button"
                  class="danger"
                  @click="promptDelete(product)"
                >
                  Delete
                </button>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </section>

    <div v-if="addModal.visible" class="modal-overlay" @click.self="closeAddModal">
      <div class="modal">
        <header>
          <h2>เพิ่มรหัสสินค้า</h2>
          <p>ความยาว 30 ตัวอักษร (A-Z, 0-9)</p>
        </header>
        <form class="modal-body" @submit.prevent="submitNewProduct">
          <label for="modal-code-input">รหัสสินค้า</label>
          <input
            id="modal-code-input"
            type="text"
            :value="addModal.code"
            @input="handleAddInput"
            @keydown="restrictAddInput"
            placeholder="ABCDE-ABCDE-ABCDE-ABCDE-ABCDE-ABCDE"
            autocomplete="off"
          />
          <p v-if="addModal.error" class="error-message">{{ addModal.error }}</p>
          <div class="modal-actions">
            <button type="button" class="ghost" @click="closeAddModal">
              ยกเลิก
            </button>
            <button type="submit" class="primary" :disabled="!canAddSubmit">
              {{ addModal.submitting ? 'กำลังบันทึก...' : 'บันทึก' }}
            </button>
          </div>
        </form>
      </div>
    </div>

    <div v-if="qrModal.visible" class="modal-overlay" @click.self="closeQrModal">
      <div class="modal">
        <header>
          <h2>QR Code</h2>
          <p>{{ qrModal.product?.code }}</p>
        </header>
        <div class="modal-body">
          <p v-if="qrModal.loading">กำลังสร้าง QR Code...</p>
          <p v-else-if="qrModal.error" class="error-message">
            {{ qrModal.error }}
          </p>
          <img
            v-else
            :src="qrModal.imageUrl"
            alt="QR Code"
            class="qr-image"
          />
        </div>
        <button type="button" class="ghost" @click="closeQrModal">ปิด</button>
      </div>
    </div>

    <div
      v-if="deleteModal.visible"
      class="modal-overlay"
      @click.self="cancelDelete"
    >
      <div class="modal">
        <header>
          <h2>ยืนยันการลบ</h2>
        </header>
        <div class="modal-body">
          <p>
            ต้องการลบรหัสสินค้า
            <strong>{{ deleteModal.product?.code }}</strong>
            หรือไม่?
          </p>
          <p v-if="deleteModal.error" class="error-message">
            {{ deleteModal.error }}
          </p>
        </div>
        <div class="modal-actions">
          <button type="button" class="ghost" @click="cancelDelete">
            ยกเลิก
          </button>
          <button
            type="button"
            class="danger"
            :disabled="deleteModal.loading"
            @click="confirmDelete"
          >
            {{ deleteModal.loading ? 'กำลังลบ...' : 'ยืนยัน' }}
          </button>
        </div>
      </div>
    </div>
  </div>
</template>
